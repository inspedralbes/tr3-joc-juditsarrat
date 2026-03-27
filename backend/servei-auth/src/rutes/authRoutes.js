const express = require('express');
const router = express.Router();


const UserRepository = require('../repositoris/UserRepository');
const AuthService = require('../serveis/AuthService');
const AuthController = require('../controllers/AuthController');
const authMiddleware = require('../middlewares/authMiddleware');

// Injecció de dependències)
const repo = new UserRepository();
const authService = new AuthService(repo);
const authController = new AuthController(authService);


router.post('/register', authController.register);


router.post('/login', authController.login);

// Ruta per obtenir dades de l'usuari actual 
router.get('/me', authMiddleware, authController.me);

module.exports = router;
