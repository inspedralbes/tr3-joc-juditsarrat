const express = require('express');
const router = express.Router();


const UserRepository = require('../repositoris/UserRepository');
const AuthService = require('../serveis/AuthService');
const AuthController = require('../controllers/AuthController');
const authMiddleware = require('../middlewares/authMiddleware');


const repo = new UserRepository();
const authService = new AuthService(repo);
const authController = new AuthController(authService);




router.post('/register', authController.register);


router.post('/login', authController.login);


router.get('/me', authMiddleware, authController.me);

module.exports = router;
