
class AuthController {
    constructor(authService) {
        this.authService = authService;

        this.register = this.register.bind(this);
        this.login = this.login.bind(this);
        this.me = this.me.bind(this);
    }

    async register(req, res) {
        try {
            const username = req.body.username;
            const email = req.body.email;
            const password = req.body.password;

            const user = await this.authService.register(username, email, password);

            return res.status(201).json({
                message: "Usuari registrat amb èxit.",
                user: user
            });
        } catch (err) {
            const message = err.message;

            
            if (message === "Tots els camps (username, email, password) són obligatoris.") {
                return res.status(400).json({ error: message });
            }
            if (message === "La contrasenya ha de tenir almenys 6 caràcters.") {
                return res.status(400).json({ error: message });
            }
            if (message === "El nom d'usuari ja està en ús.") {
                return res.status(400).json({ error: message });
            }
            if (message === "El correu electrònic ja està registrat.") {
                return res.status(400).json({ error: message });
            }
console.error("ERROR REGISTER:", err);
            return res.status(500).json({ error: "Error intern durant el registre." });
        }
    }

    async login(req, res) {
    try {
        const email = req.body.email;
        const password = req.body.password;
        
        // ✅ Ahora authService.login retorna { token, user }
        const { token, user } = await this.authService.login(email, password);
        
        return res.status(200).json({
            message: "Login correcte.",
            token: token,
            user: user  // ✅ Añadido
        });
    } catch (err) {
        const message = err.message;
        if (message === "Credencials incorrectes.") {
            return res.status(401).json({ error: message });
        }
        if (message === "Cal proporcionar email i contrasenya.") {
            return res.status(400).json({ error: message });
        }
        console.error("ERROR LOGIN:", err);
        return res.status(500).json({ error: "Error intern durant el login." });
    }
}
    async me(req, res) {
        try {
            const userId = req.user.id;
            const user = await this.authService.getUserById(userId);

            return res.status(200).json({ user: user });
        } catch (err) {
            if (err.message === "Usuari no trobat.") {
                return res.status(404).json({ error: err.message });
            }
            console.error("ERROR REGISTER:", err);
            return res.status(500).json({ error: "Error intern en obtenir les dades." });
        }
    }
}

module.exports = AuthController;
