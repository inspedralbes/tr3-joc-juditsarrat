const express = require('express');
const router = express.Router();
const JocController = require('../controllers/JocController');
const JocService = require('../serveis/JocService');
const GameRepository = require('../repositoris/GameRepository');

// Crear instancias
const gameRepository = new GameRepository();
const jocService = new JocService(gameRepository);
const jocController = new JocController(jocService);

// Crear una nova partida
router.post('/', (req, res) => jocController.crear(req, res));

// Llistar partides en espera
router.get('/', (req, res) => jocController.llistarDisponibles(req, res));

// ✅ OBTENER POR CÓDIGO ANTES QUE POR ID
router.get('/code/:code', (req, res) => jocController.obtenirPerCodi(req, res));

// Obtenir l'estat d'una partida específica
router.get('/:id', (req, res) => jocController.obtenirEstat(req, res));

// Unir-se a una partida
router.post('/:id/join', (req, res) => jocController.unirse(req, res));

// Iniciar una partida
router.post('/:id/start', (req, res) => jocController.iniciar(req, res));

module.exports = router;