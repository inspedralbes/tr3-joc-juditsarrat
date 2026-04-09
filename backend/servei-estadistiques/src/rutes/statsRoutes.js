const express = require('express');
const router = express.Router();

function createStatsRouter(statsController) {

    router.get('/jugador/:id', function (req, res) {
        statsController.obtenirJugador(req, res);
    });


    router.get('/partida/:id', function (req, res) {
        statsController.obtenirPartida(req, res);
    });


    router.get('/ranquing', function (req, res) {
        statsController.obtenirRanquing(req, res);
    });


    router.post('/resultat', function (req, res) {
        statsController.guardarResultat(req, res);
    });

    return router;
}

module.exports = createStatsRouter;
