
class IUserRepository {
    constructor() {
        if (this.constructor === IUserRepository) {
            throw new Error("No es pot instanciar una classe abstracta.");
        }
    }

    async findById(id) {
        throw new Error("Mètode 'findById()' no implementat.");
    }


    async findByEmail(email) {
        throw new Error("Mètode 'findByEmail()' no implementat.");
    }


    async findByUsername(username) {
        throw new Error("Mètode 'findByUsername()' no implementat.");
    }

    async create(userData) {
        throw new Error("Mètode 'create()' no implementat.");
    }


    async update(id, userData) {
        throw new Error("Mètode 'update()' no implementat.");
    }


    async delete(id) {
        throw new Error("Mètode 'delete()' no implementat.");
    }
}

module.exports = IUserRepository;
