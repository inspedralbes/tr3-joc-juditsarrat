const WebSocket = require('ws');

class WebSocketService {
    constructor(wss, jocService) {
        this.wss = wss;
        this.jocService = jocService;
        this.partides = {};
        this.inicialitzarEvents();
    }

    inicialitzarEvents() {
        const self = this;
        this.wss.on('connection', function (socket) {
            console.log("✅ Nova connexió WebSocket establerta.");
            
            socket.on('message', async function (data) {
                try {
                    const message = JSON.parse(data);
                    await self.gestionarMissatge(socket, message);
                } catch (err) {
                    console.error("❌ Error processant missatge WS:", err.message);
                    socket.send(JSON.stringify({ type: 'error', message: "Format no vàlid." }));
                }
            });
            
            socket.on('close', function () {
                self.gestionarDesconnexio(socket);
            });
        });
    }

    async gestionarMissatge(socket, message) {
        const type = message.type;
        const payload = message.payload;

        if (type === 'join-game') {
            const gameId = payload.gameId;
            const playerId = payload.playerId;
            
            socket.gameId = gameId;
            socket.playerId = playerId;

            if (!this.partides[gameId]) {
                this.partides[gameId] = [];
            }
            
            this.partides[gameId].push(socket);
            
            console.log("👤 Jugador " + playerId + " s'ha unit a la partida " + gameId);
            console.log("📊 Total jugadors en sala: " + this.partides[gameId].length);
            
            // ✅ NOTIFICAR A TOTS ELS CLIENTS DE LA SALA
            this.broadcast(gameId, {
                type: 'player-joined',
                payload: { 
                    playerId: playerId,
                    totalPlayers: this.partides[gameId].length
                }
            });
            
        } else if (type === 'player-action') {
            const gameId = socket.gameId;
            const playerId = socket.playerId;
            const accio = payload.accio;

            if (!gameId || !playerId) {
                return socket.send(JSON.stringify({ type: 'error', message: "No estàs unit a cap partida." }));
            }

            try {
                const updatedState = await this.jocService.processarAccio(gameId, playerId, accio);
                this.broadcast(gameId, {
                    type: 'game-update',
                    payload: updatedState
                });
            } catch (err) {
                socket.send(JSON.stringify({ type: 'error', message: err.message }));
            }
        }
    }

    broadcast(gameId, message) {
        const sockets = this.partides[gameId];
        
        if (sockets) {
            const data = JSON.stringify(message);
            
            for (let i = 0; i < sockets.length; i++) {
                const s = sockets[i];
                if (s.readyState === WebSocket.OPEN) {
                    s.send(data);
                }
            }
        }
    }

    gestionarDesconnexio(socket) {
        const gameId = socket.gameId;
        const playerId = socket.playerId;

        if (gameId && this.partides[gameId]) {
            const index = this.partides[gameId].indexOf(socket);
            
            if (index !== -1) {
                this.partides[gameId].splice(index, 1);
            }
            
            console.log("👋 Jugador " + playerId + " s'ha desconnectat. Total: " + this.partides[gameId].length);
            
            this.broadcast(gameId, {
                type: 'player-disconnected',
                payload: { 
                    playerId: playerId,
                    totalPlayers: this.partides[gameId].length
                }
            });

            if (this.partides[gameId].length === 0) {
                delete this.partides[gameId];
                console.log("🗑️ Sala " + gameId + " eliminada (0 jugadores)");
            }
        }
    }
}

module.exports = WebSocketService;