const express = require('express');
const mongoose = require('mongoose');
const dotenv = require('dotenv');


dotenv.config();


const ResultRepository = require('./repositoris/ResultRepository');
const UserStatsRepository = require('./repositoris/UserStatsRepository');
const EstadistiquesService = require('./serveis/EstadistiquesService');
const StatsController = require('./controllers/StatsController');
const createStatsRouter = require('./rutes/statsRoutes');

const app = express();
const PORT = process.env.PORT || 3003;


app.use(express.json());


const mongoURI = process.env.MONGODB_URI || 'mongodb://localhost:27017/bomberman_stats';
mongoose.connect(mongoURI)
    .then(function () {
        console.log("Connectat a MongoDB (Servei Estadístiques)");
    })
    .catch(function (err) {
        console.error("Error connectant a MongoDB:", err);
    });


const resultRepo = new ResultRepository();
const userStatsRepo = new UserStatsRepository();
const statsService = new EstadistiquesService(resultRepo, userStatsRepo);
const statsController = new StatsController(statsService);


app.use('/estadistiques', createStatsRouter(statsController));


app.get('/', function (req, res) {
    res.send("Servei d'Estadístiques Bomberman Funcionant!");
});


app.listen(PORT, function () {
    console.log("Servidor d'Estadístiques escoltant al port " + PORT);
});
