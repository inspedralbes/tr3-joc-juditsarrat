require('dotenv').config();
const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
const authRoutes = require('./rutes/authRoutes');

const app = express();

// 1. Middlewares bàsics
app.use(cors());
app.use(express.json());

// 2. Connexió a MongoDB
const mongoUri = process.env.MONGODB_URI;

mongoose.connect(mongoUri)
    .then(function () {
        console.log("Connectat a MongoDB correctament.");
    })
    .catch(function (err) {
        console.error("Error connectant a MongoDB:", err.message);
    });

// 3. Rutes
app.use('/auth', authRoutes);

// 4. Ruta de prova per saber si el servidor funciona
app.get('/', function (req, res) {
    res.send("El servei d'autenticació està funcionant.");
});

// 5. Gestió d'errors global (molt senzilla)
app.use(function (err, req, res, next) {
    console.error("Error global detectat:", err.stack);
    res.status(500).json({
        error: "S'ha produït un error intern al servidor."
    });
});

// 6. Escoltant al port
const port = process.env.PORT || 3001;
app.listen(port, function () {
    console.log("Servidor d'autenticació escoltant al port " + port);
});
