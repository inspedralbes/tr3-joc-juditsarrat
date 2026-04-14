const express = require('express');
const http = require('http');
const cors = require('cors');
const WebSocket = require('ws');
const mongoose = require('mongoose');
require('dotenv').config();

const app = express();
const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

// Middleware
app.use(cors());
app.use(express.json());

// Conectar MongoDB
mongoose.connect(process.env.MONGODB_URI || 'mongodb://mongodb:27017/bomberman')
    .then(() => console.log('[MongoDB] ✅ Conectado'))
    .catch(err => console.error('[MongoDB] ❌ Error:', err));

// Routes HTTP
const jocRutes = require('../rutes/jocRutes');
app.use('/games', jocRutes);

// WebSocket - Mapa de conexiones por gameId
const gameConnections = new Map();

wss.on('connection', (ws, req) => {
    console.log('[WebSocket] ✅ Cliente conectado');

    // Obtener gameId y playerId de la URL
    const url = new URL(req.url, `http://${req.headers.host}`);
    const gameId = url.searchParams.get('gameId');
    const playerId = url.searchParams.get('playerId');

    if (!gameId || !playerId) {
        console.log('[WebSocket] ❌ gameId o playerId faltante');
        ws.close(1008, 'gameId y playerId requeridos');
        return;
    }

    console.log(`[WebSocket] 🎮 Game: ${gameId}, Player: ${playerId}`);

    // Registrar conexión
    if (!gameConnections.has(gameId)) {
        gameConnections.set(gameId, []);
    }
    gameConnections.get(gameId).push({
        ws: ws,
        playerId: playerId
    });

    const playerCount = gameConnections.get(gameId).length;
    console.log(`[WebSocket] 👥 Total jugadores en ${gameId}: ${playerCount}`);

    // Notificar a otros que alguien se conectó
    broadcastToGame(gameId, {
        type: 'player-joined',
        playerId: playerId,
        totalPlayers: playerCount
    });

    // Si hay 2 jugadores, iniciar el juego (una sola vez)
    if (playerCount === 2) {
        console.log(`[WebSocket] 🎮 ¡Juego ${gameId} listo para empezar!`);

        const players = gameConnections.get(gameId).map(c => c.playerId);

        setTimeout(() => {
            broadcastToGame(gameId, {
                type: 'game-started',
                gameId: gameId,
                players: players,
                totalPlayers: 2
            });
        }, 500);
    }

    // Manejar mensajes
    ws.on('message', (message) => {
        console.log(`[WebSocket] De ${playerId}: ${message}`);

        try {
            const data = JSON.parse(message);
            handleGameMessage(gameId, playerId, data);
        } catch (e) {
            console.error('[WebSocket] Error parseando:', e.message);
        }
    });

    // Manejar desconexión
    ws.on('close', () => {
        console.log(`[WebSocket] 👋 ${playerId} desconectado`);

        const connections = gameConnections.get(gameId);
        if (connections) {
            const index = connections.findIndex(c => c.playerId === playerId);
            if (index > -1) {
                connections.splice(index, 1);
            }

            const remainingPlayers = connections.length;
            broadcastToGame(gameId, {
                type: 'player-disconnected',
                playerId: playerId,
                totalPlayers: remainingPlayers
            });

            console.log(`[WebSocket] 👥 Jugadores restantes en ${gameId}: ${remainingPlayers}`);
        }
    });

    ws.on('error', (error) => {
        console.error(`[WebSocket] ❌ Error ${playerId}:`, error.message);
    });
});

// Funciones auxiliares
function broadcastToGame(gameId, message) {
    const connections = gameConnections.get(gameId);
    if (!connections) return;

    const json = JSON.stringify(message);
    let sentCount = 0;

    connections.forEach(client => {
        if (client.ws.readyState === WebSocket.OPEN) {
            client.ws.send(json);
            sentCount++;
        }
    });

    console.log(`[WebSocket]  Broadcast a ${gameId}: ${sentCount} clientes`);
}

function handleGameMessage(gameId, playerId, data) {
    switch (data.type) {
        case 'join-game':
            console.log(`[WebSocket] ${playerId} entró a sala ${gameId}`);
            broadcastToGame(gameId, {
                type: 'player-joined',
                playerId: playerId,
                totalPlayers: gameConnections.get(gameId).length
            });
            break;

        case 'player-move':
            broadcastToGame(gameId, {
                type: 'player-moved',
                playerId: playerId,
                x: data.x,
                y: data.y
            });
            break;

        case 'place-bomb':
            broadcastToGame(gameId, {
                type: 'bomb-placed',
                playerId: playerId,
                x: data.x,
                y: data.y
            });
            break;

        default:
            console.log(`[WebSocket] ⚠️ Tipo desconocido: ${data.type}`);
    }
}

// Error handling
app.use((err, req, res, next) => {
    console.error('[Error]', err);
    res.status(500).json({ error: err.message });
});

// Iniciar servidor
const PORT = process.env.PORT || 3002;
server.listen(PORT, () => {
    console.log(`\n[✅ Servidor] Escuchando en puerto ${PORT}`);
    console.log(`[✅ WebSocket] Disponible en ws://localhost:${PORT}\n`);
});

module.exports = app;