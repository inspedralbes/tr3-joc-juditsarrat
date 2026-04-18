const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');


class AuthService {
    constructor(userRepository) {
        this.userRepository = userRepository;
        this.jwtSecret = process.env.JWT_SECRET || 'secret_per_defecte';
        this.saltRounds = 10;
    }

    async register(username, email, password) {


        console.log("REGISTER:", username, email, password);
        if (username === "" || email === "" || password === "") {
            throw new Error("Tots els camps (username, email, password) són obligatoris.");
        }

        if (password.length < 6) {
            throw new Error("La contrasenya ha de tenir almenys 6 caràcters.");
        }


        const userByUsername = await this.userRepository.findByUsername(username);
        if (userByUsername !== null) {
            throw new Error("El nom d'usuari ja està en ús.");
        }

        const userByEmail = await this.userRepository.findByEmail(email);
        if (userByEmail !== null) {
            throw new Error("El correu electrònic ja està registrat.");
        }

        const passwordHash = await bcrypt.hash(password, this.saltRounds);


        const userData = {
            username: username,
            email: email,
            passwordHash: passwordHash
        };

        const newUser = await this.userRepository.create(userData);


        const publicUser = {
            id: newUser._id || newUser.id,
            username: newUser.username,
            email: newUser.email
        };

        return publicUser;
    }

    async login(email, password) {
        if (email === "" || password === "") {
            throw new Error("Cal proporcionar email i contrasenya.");
        }
        const user = await this.userRepository.findByEmail(email);
        console.log("USUARI TROBAT:", user);
        console.log("PASSWORD HASH:", user?.passwordHash);
        if (user === null) {
            throw new Error("Credencials incorrectes.");
        }
        const isMatch = await bcrypt.compare(password, user.passwordHash);
        if (isMatch === false) {
            throw new Error("Credencials incorrectes.");
        }

        const payload = {
            id: user._id || user.id,
            username: user.username,
            email: user.email
        };
        const token = jwt.sign(payload, this.jwtSecret, { expiresIn: '1h' });


        const publicUser = {
            id: user._id || user.id,
            username: user.username,
            email: user.email
        };

        return { token, user: publicUser };
    }

    async getUserById(id) {
        const user = await this.userRepository.findById(id);

        if (user === null) {
            throw new Error("Usuari no trobat.");
        }

        const publicUser = {
            id: user._id || user.id,
            username: user.username,
            email: user.email
        };

        return publicUser;
    }
}

module.exports = AuthService;
