
class UserService {

    constructor(userRepository) {
        this.userRepository = userRepository;
    }

    async registerUser(username, email, passwordHash) {

        const existingUser = await this.userRepository.findByEmail(email);
        if (existingUser) {
            throw new Error("L'usuari ja existeix amb aquest correu.");
        }

        // Crear l'usuari usant el repositori
        return await this.userRepository.create({
            username,
            email,
            passwordHash
        });
    }

    async getUserInfo(id) {
        const user = await this.userRepository.findById(id);
        if (!user) {
            throw new Error("Usuari no trobat.");
        }
        return user;
    }
}

module.exports = UserService;
