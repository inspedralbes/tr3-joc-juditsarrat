const jwt = require('jsonwebtoken');


function authMiddleware(req, res, next) {
    const authHeader = req.headers['authorization'];

    if (authHeader === undefined || authHeader === null) {
        return res.status(401).json({
            error: "No s'ha proporcionat cap token d'autorització."
        });
    }

    const parts = authHeader.split(' ');

    if (parts.length !== 2) {
        return res.status(401).json({
            error: "Format de token no vàlid. S'espera 'Bearer <token>'."
        });
    }

    const token = parts[1];
    const jwtSecret = process.env.JWT_SECRET || 'secret_per_defecte';

    try {

        const decoded = jwt.verify(token, jwtSecret);


        req.user = decoded;


        next();
    } catch (err) {
        return res.status(401).json({
            error: "Token no vàlid o caducat."
        });
    }
}

module.exports = authMiddleware;
