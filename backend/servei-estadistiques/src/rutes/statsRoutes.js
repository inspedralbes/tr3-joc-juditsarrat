const express = require('express');
const router = express.Router();

function createStatsRouter(statsController) {

    router.get('/partida/:id', function (req, res) {
        statsController.obtenirPartida(req, res);
    });

    router.post('/resultat', function (req, res) {
        statsController.guardarResultat(req, res);
    });

    return router;
}

module.exports = createStatsRouter;
