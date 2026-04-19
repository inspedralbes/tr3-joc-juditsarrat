// Controlador per gestionar les peticions d'estadístiques simplificat.

class StatsController {
    constructor(estadistiquesService) {
        this.estadistiquesService = estadistiquesService;
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
