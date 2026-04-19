// Controlador per gestionar les peticions d'estadístiques.

class StatsController {
    constructor(estadistiquesService) {
        this.estadistiquesService = estadistiquesService;
    }

    //estadistiques de jugador per id
    async obtenirJugador(req, res) {
        try {
            const userId = req.params.id;
            const stats = await this.estadistiquesService.obtenirEstadistiquesJugador(userId);
            res.status(200).json(stats);
        } catch (err) {
            res.status(404).json({ message: err.message });
        }
    }

    async obtenirPartida(req, res) {
        try {
            const gameId = req.params.id;
            const result = await this.estadistiquesService.obtenirResultatsPartida(gameId);
            res.status(200).json(result);
        } catch (err) {
            res.status(404).json({ message: err.message });
        }
    }

    async obtenirRanquing(req, res) {
        try {
            const limit = parseInt(req.query.limit) || 10;
            const ranking = await this.estadistiquesService.obtenirRanquing(limit);
            res.status(200).json(ranking);
        } catch (err) {
            res.status(500).json({ message: err.message });
        }
    }

    async guardarResultat(req, res) {
        try {
            const { gameId, winnerId, players, duration } = req.body;

            if (!gameId || !winnerId || !players) {
                return res.status(400).json({ message: "Falten dades obligatòries." });
            }

            const result = await this.estadistiquesService.guardarResultat(gameId, winnerId, players, duration);
            res.status(201).json(result);
        } catch (err) {
            console.error(err);
            res.status(500).json({ message: err.message });
        }
    }
}

module.exports = StatsController;
