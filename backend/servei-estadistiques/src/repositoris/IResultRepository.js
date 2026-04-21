/**
 * Interfície per al repositori de resultats de partides.
 */
class IResultRepository {
    async findById(id) {
        throw new Error("Mètode findById() no implementat.");
    }

    async findByGameId(gameId) {
        throw new Error("Mètode findByGameId() no implementat.");
    }

    async findByPlayerId(playerId) {
        throw new Error("Mètode findByPlayerId() no implementat.");
    }

    async create(resultData) {
        throw new Error("Mètode create() no implementat.");
    }
}

module.exports = IResultRepository;
