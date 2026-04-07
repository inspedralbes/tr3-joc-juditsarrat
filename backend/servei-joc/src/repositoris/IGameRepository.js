//Interfície (contracte) per al repositori de partides.
//Defineix els mètodes que qualsevol implementació ha de tenir.

class IGameRepository {
    async findById(id) {
        throw new Error("Mètode findById() no implementat.");
    }

    async findAll() {
        throw new Error("Mètode findAll() no implementat.");
    }

    async findByStatus(status) {
        throw new Error("Mètode findByStatus() no implementat.");
    }

    async create(gameData) {
        throw new Error("Mètode create() no implementat.");
    }

    async update(id, gameData) {
        throw new Error("Mètode update() no implementat.");
    }

    async addPlayer(gameId, playerId) {
        throw new Error("Mètode addPlayer() no implementat.");
    }

    async updateStatus(gameId, status) {
        throw new Error("Mètode updateStatus() no implementat.");
    }

    async delete(id) {
        throw new Error("Mètode delete() no implementat.");
    }
}

module.exports = IGameRepository;
