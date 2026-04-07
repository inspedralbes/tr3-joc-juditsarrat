
// Servei que gestiona la lògica de les estadístiques i resultats.

class EstadistiquesService {
    constructor(resultRepository, userStatsRepository) {
        this.resultRepository = resultRepository;
        this.userStatsRepository = userStatsRepository;
    }


    //Desa el resultat d'una partida i actualitza les estadístiques dels jugadors.

    async guardarResultat(gameId, winnerId, players, duration) {

        const resultData = {
            gameId: gameId,
            winnerId: winnerId,
            players: players,
            duration: duration
        };
        const newResult = await this.resultRepository.create(resultData);


        for (let i = 0; i < players.length; i++) {
            const p = players[i];
            await this.actualitzarEstadistiquesUsuari(p.playerId, p.score, p.playerId === winnerId);
        }

        return newResult;
    }


    //Lògica interna per actualitzar o crear les estadístiques d'un usuari.

    async actualitzarEstadistiquesUsuari(userId, score, esGuanyador) {
        let stats = await this.userStatsRepository.findByUserId(userId);

        if (!stats) {

            stats = {
                userId: userId,
                totalGames: 1,
                wins: esGuanyador ? 1 : 0,
                losses: esGuanyador ? 0 : 1,
                totalScore: score,
                avgScore: score
            };
            return await this.userStatsRepository.create(stats);
        } else {

            const nousValors = {
                totalGames: stats.totalGames + 1,
                wins: esGuanyador ? stats.wins + 1 : stats.wins,
                losses: esGuanyador ? stats.losses : stats.losses + 1,
                totalScore: stats.totalScore + score,
            };
            nousValors.avgScore = nousValors.totalScore / nousValors.totalGames;

            return await this.userStatsRepository.updateStats(userId, nousValors);
        }
    }


    //Retorna les estadístiques agregades d'un jugador.

    async obtenirEstadistiquesJugador(userId) {
        const stats = await this.userStatsRepository.findByUserId(userId);
        if (!stats) {
            throw new Error("L'usuari no té estadístiques registrades.");
        }
        return stats;
    }


    //Retorna el resultat detallat d'una partida concreta.

    async obtenirResultatsPartida(gameId) {
        const result = await this.resultRepository.findByGameId(gameId);
        if (!result) {
            throw new Error("No s'ha trobat el resultat per aquesta partida.");
        }
        return result;
    }


    //Retorna el rànquing dels millors N jugadors (per victòries i puntuació).

    async obtenirRanquing(limit) {
        return await this.userStatsRepository.getTopPlayers(limit);
    }
}

module.exports = EstadistiquesService;
