
// Servei que gestiona la lògica de les estadístiques i resultats.

class EstadistiquesService {
    constructor(resultRepository) {
        this.resultRepository = resultRepository;
    }


    //Desa el resultat d'una partida.

    async guardarResultat(gameId, winnerId, players, duration) {

        const resultData = {
            gameId: gameId,
            winnerId: winnerId,
            players: players,
            duration: duration
        };
        
        console.log(`[StatsService] Guardant resultat per partida: ${gameId}`);
        const newResult = await this.resultRepository.create(resultData);
        console.log(`[StatsService] ✅ Resultat guardat a la col·lecció 'results'`);

        return newResult;
    }


    //Retorna el resultat detallat d'una partida concreta.

    async obtenirResultatsPartida(gameId) {
        const result = await this.resultRepository.findByGameId(gameId);
        if (!result) {
            throw new Error("No s'ha trobat el resultat per aquesta partida.");
        }
        return result;
    }
}

module.exports = EstadistiquesService;
