
//Controlador per gestionar les peticions HTTP relacionades amb el joc.

class JocController {
    constructor(jocService) {
        this.jocService = jocService;


        this.crear = this.crear.bind(this);
        this.llistarDisponibles = this.llistarDisponibles.bind(this);
        this.obtenirEstat = this.obtenirEstat.bind(this);
        this.unirse = this.unirse.bind(this);
        this.iniciar = this.iniciar.bind(this);
        this.enviarAccio = this.enviarAccio.bind(this);
        this.obtenirResultats = this.obtenirResultats.bind(this);
    }

  async crear(req, res) {
    console.log("Petició rebuda a crear:", req.method, req.url);
    console.log("Headers:", req.headers);
    console.log("Body:", req.body);
    
    try {
        const hostId = req.body.hostId;
        const config = req.body.config;
        
        if (!hostId) {
            return res.status(400).json({ error: 'hostId és obligatori' });
        }
        
        const game = await this.jocService.crearPartida(hostId, config);
        
        // ✅ Convierte el ObjectId a string
        const gameResponse = {
            _id: game._id.toString(),
            hostId: game.hostId,
            status: game.status,
            players: game.players,
            config: game.config,
            createdAt: game.createdAt,
            updatedAt: game.updatedAt,
            __v: game.__v
        };
        
        console.log("✅ Resposta enviada:", gameResponse);
        return res.status(201).json(gameResponse);
        
    } catch (err) {
        console.error("❌ Error en JocController.crear:", err);
        return res.status(500).json({ error: "Error en crear la partida." });
    }
}

    async llistarDisponibles(req, res) {
        try {
            const games = await this.jocService.llistarPartidesDisponibles();
            return res.status(200).json(games);
        } catch (err) {
            return res.status(500).json({ error: "Error en llistar les partides." });
        }
    }

    async obtenirEstat(req, res) {
        try {
            const gameId = req.params.id;
            const game = await this.jocService.obtenirEstat(gameId);
            return res.status(200).json(game);
        } catch (err) {
            if (err.message === "La partida no existeix.") {
                return res.status(404).json({ error: err.message });
            }
            return res.status(500).json({ error: "Error en obtenir l'estat." });
        }
    }

    async unirse(req, res) {
    try {
        const gameId = req.params.id;
        const playerId = req.body.playerId;
        
        if (!playerId) {
            return res.status(400).json({ error: 'playerId es obligatorio' });
        }
        
        console.log("Jugador " + playerId + " uniéndose a sala: " + gameId);
        
        const game = await this.jocService.unirsePartida(gameId, playerId);
        
        // ✅ Convierte a formato JSON válido
        const gameResponse = {
            _id: game._id.toString(),
            hostId: game.hostId,
            status: game.status,
            players: game.players,
            config: game.config,
            createdAt: game.createdAt,
            updatedAt: game.updatedAt
        };
        
        console.log("✅ Jugador unido. Total jugadores:", game.players.length);
        return res.status(200).json(gameResponse);
        
    } catch (err) {
        console.error("Error:", err);
        return res.status(400).json({ error: err.message });
    }
}
    async iniciar(req, res) {
        try {
            const gameId = req.params.id;
            const hostId = req.body.hostId;
            const game = await this.jocService.iniciarPartida(gameId, hostId);
            return res.status(200).json(game);
        } catch (err) {
            return res.status(400).json({ error: err.message });
        }
    }

    async enviarAccio(req, res) {
        try {
            const gameId = req.params.id;
            const playerId = req.body.playerId;
            const accio = req.body.accio;
            const game = await this.jocService.processarAccio(gameId, playerId, accio);
            return res.status(200).json(game);
        } catch (err) {
            return res.status(400).json({ error: err.message });
        }
    }

    async obtenirResultats(req, res) {
        try {
            const gameId = req.params.id;

            const game = await this.jocService.obtenirEstat(gameId);
            if (game.status !== 'finished') {
                return res.status(400).json({ error: "La partida encara no ha acabat." });
            }
            return res.status(200).json(game);
        } catch (err) {
            return res.status(500).json({ error: "Error en obtenir els resultats." });
        }
    }
}

module.exports = JocController;
