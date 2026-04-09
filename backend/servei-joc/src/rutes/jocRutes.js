const express = require('express');
const router = express.Router();
const JocController = require('../controllers/JocController');
const JocService = require('../serveis/JocService');
const GameRepository = require('../repositoris/GameRepository');

// Crear instancias
const gameRepository = new GameRepository();
const jocService = new JocService(gameRepository);
const jocController = new JocController(jocService);

// Log para depuración
router.use((req, res, next) => {
    console.log(`[jocRutes] ${req.method} ${req.url}`);
    next();
});

// Crear una nova partida
router.post('/', jocController.crear);

// Llistar partides en espera
router.get('/', jocController.llistarDisponibles);

// Obtenir l'estat d'una partida específica
router.get('/:id', jocController.obtenirEstat);

// Unir-se a una partida
router.post('/:id/join', jocController.unirse);

// Iniciar una partida
router.post('/:id/start', jocController.iniciar);

module.exports = router;