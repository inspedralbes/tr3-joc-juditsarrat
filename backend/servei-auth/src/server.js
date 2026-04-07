require('dotenv').config();
const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
const authRoutes = require('./rutes/authRoutes');

const app = express();


app.use(cors());
app.use(express.json());


const mongoUri = process.env.MONGODB_URI;

mongoose.connect(mongoUri)
    .then(function () {
        console.log("Connectat a MongoDB correctament.");
    })
    .catch(function (err) {
        console.error("Error connectant a MongoDB:", err.message);
    });


app.use('/auth', authRoutes);


app.get('/', function (req, res) {
    res.send("El servei d'autenticació està funcionant.");
});


app.use(function (err, req, res, next) {
    console.error("Error global detectat:", err.stack);
    res.status(500).json({
        error: "S'ha produït un error intern al servidor."
    });
});


const port = process.env.PORT || 3001;
app.listen(port, function () {
    console.log("Servidor d'autenticació escoltant al port " + port);
});
