# BomberMan — Projecte Transversal 3
 
Joc multijugador *BomberMan* desenvolupat com a **Projecte Transversal 3 (TR3)** a l'Institut Pedralbes. Inclou un client de joc fet amb **Unity ** i un backend amb arquitectura de **microserveis** en **Node.js / Express**, orquestrat amb **Docker Compose** i un *gateway* **Nginx**. També incorpora entrenament d'IA amb **Unity ML-Agents**.
 
## Integrants
 
- **Judit Sarrat Andujar** ([@a24judsarand](https://github.com/a24judsarand))
 
## Descripció
 
El joc permet als jugadors crear o unir-se a partides multijugador en línia, autenticar-se amb un compte propi i competir entre ells. Un cop finalitzada la partida es guarden els resultats i s'actualitza el rànquing global de jugadors. El projecte inclou, a més, un entorn d'entrenament amb ML-Agents per a agents d'IA que aprenen a jugar a BomberMan.
 
Funcionalitats principals:
 
- Registre i inici de sessió d'usuaris amb JWT.
- Creació de partides, llistat de partides disponibles i unió a partides per codi.
- Comunicació en temps real entre client i servidor via **WebSockets**.
- Estadístiques de jugadors i partides amb rànquing global.
- Escena d'entrenament amb **Unity ML-Agents** per a agents d'IA.
 
## Enllaços del projecte
 
- **Repositori:** https://github.com/inspedralbes/tr3-joc-juditsarrat
- **Gestor de tasques (Taiga):*https://tree.taiga.io/project/a24judsarand-bomberman/backlog*
 
## Arquitectura
 
```
┌──────────────────────┐
│  Client Unity (.0)  │
│  - LoginScene        │
│  - MenuPrincipal     │
│  - GameScene         │
│  - TrainingScene
│  - GameOverScene
└──────────┬───────────┘
           │ HTTP + WebSocket
           ▼
┌──────────────────────┐          ┌─────────────────────────┐
│  Gateway (Nginx)     │          │  MongoDB Atlas          │
│  :8080               │          │  Base de dades          │
│  /auth/              │          └──────────▲──────────────┘
│  /joc/               │                     │
│  /estadistiques/     │                     │
└──┬───────┬────────┬──┘                     │
   │       │        │                        │
   ▼       ▼        ▼                        │
┌──────┐┌──────┐┌────────────────┐           │
│ auth ││ joc  ││ estadistiques  │───────────┘
│:3001 ││:3002 ││ :3003          │
└──────┘└──────┘└────────────────┘
```
 
### Tecnologies
 
| Component            | Tecnologia                                               |
|----------------------|----------------------------------------------------------|
| Client de joc        | Unity , C#                                |
| IA                   | Unity **ML-Agents**                                      |
| Gateway              | **Nginx**                                                |
| Microserveis backend | **Node.js**, **Express **, **Mongoose**                  |
| Autenticació         | **JWT** (`jsonwebtoken`), **bcryptjs**                   |
| Temps real           | **WebSockets** (`ws`)                                    |
| Base de dades        | **MongoDB**                                              |
| Orquestració         | **Docker** + **Docker Compose**                          |
 
### Microserveis del backend
 
- **`backend/gateway`** — Nginx que exposa el port `8080` i encamina les peticions:
  - `/auth/**` → `servei-auth:3001`
  - `/joc/**` → `servei-joc:3002` (amb suport per a WebSockets)
  - `/estadistiques/**` → `servei-estadistiques:3003`
- **`backend/servei-auth`** — Registre, login i perfil d'usuari amb JWT + bcrypt.
- **`backend/servei-joc`** — Creació i gestió de partides, unió per codi i comunicació en temps real via WebSockets.
- **`backend/servei-estadistiques`** — Estadístiques per jugador i partida, i rànquing global.
- **`backend/compartit`** — Utilitats compartides entre serveis (`jwtUtils`, `validadors`, `errors`).
 
## Endpoints de l'API
 
Totes les rutes passen pel gateway a `http://localhost:8080`.
 
### `servei-auth` — `/auth`
 
| Mètode | Ruta             | Descripció                                 | Auth |
|--------|------------------|--------------------------------------------|:----:|
| POST   | `/auth/register` | Registre d'un nou usuari                   |  No  |
| POST   | `/auth/login`    | Inici de sessió (retorna JWT)              |  No  |
| GET    | `/auth/me`       | Perfil de l'usuari autenticat              |  Sí  |
 
### `servei-joc` — `/joc`
 
| Mètode | Ruta                  | Descripció                              |
|--------|-----------------------|-----------------------------------------|
| POST   | `/joc/`               | Crear una nova partida                  |
| GET    | `/joc/`               | Llistar partides en espera              |
| GET    | `/joc/code/:code`     | Obtenir partida per codi                |
| GET    | `/joc/:id`            | Obtenir l'estat d'una partida           |
| POST   | `/joc/:id/join`       | Unir-se a una partida                   |
| POST   | `/joc/:id/start`      | Iniciar una partida                     |
 
A més, el servei exposa un canal de **WebSocket** a través del mateix gateway per a la sincronització de la partida en temps real.
 
### `servei-estadistiques` — `/estadistiques`
 
| Mètode | Ruta                         | Descripció                          |
|--------|------------------------------|-------------------------------------|
| GET    | `/estadistiques/jugador/:id` | Estadístiques d'un jugador          |
| GET    | `/estadistiques/partida/:id` | Estadístiques d'una partida         |
| GET    | `/estadistiques/ranquing`    | Rànquing global de jugadors         |
| POST   | `/estadistiques/resultat`    | Guardar el resultat d'una partida   |
 
## Entorn de desenvolupament
 
### Requisits
 
- [Docker](https://docs.docker.com/get-docker/) i Docker Compose
- [Node.js](https://nodejs.org/) **>= 20** (per executar els serveis fora de Docker)
- [Unity Hub](https://unity.com/download) amb **Unity 6000.0.69f1**
- Accés a un clúster **MongoDB** 
 
### 1. Clonar el repositori
 
```bash
git clone https://github.com/inspedralbes/tr3-joc-juditsarrat.git
cd tr3-joc-juditsarrat
```
 
### 2. Backend amb Docker Compose
 
Des de l'arrel del projecte:
 
```bash
docker compose up --build
```
 
Això aixeca el gateway al port `8080` i els microserveis a la xarxa interna `bomberman-network`. Els serveis de Node no exposen ports a l'exterior; tot el trànsit entra pel gateway.
 
 
### 3. Executar un microservei sol·licitat (mode desenvolupament)
 
```bash
cd backend/servei-auth         # o servei-joc / servei-estadistiques
npm install
npm start                      # equivalent a: node src/server.js
```
 
Variables d'entorn mínimes:
 
## Estructura del repositori
 
```
.
├── BomberMan_PROJ3/          # Projecte Unity 6 (client + escena d'entrenament)
│   ├── Assets/
│   │   ├── Scenes/
│   │   ├── Scripts/
│   │   ├── Prefabs/
│   │   ├── ML-Agents/
│   │   └── ModelsIA/
│   └── ProjectSettings/
├── backend/
│   ├── gateway/              # Nginx reverse proxy
│   ├── servei-auth/          # Autenticació i usuaris
│   ├── servei-joc/           # Lògica de partides + WebSockets
│   ├── servei-estadistiques/ # Estadístiques i rànquing
│   └── compartit/            # Utilitats compartides
├── doc/                      # Documentació addicional
├── openspec/                 # Especificacions i canvis (OpenSpec)
├── docker-compose.yml        # Orquestració global
└── README.md
```
 
## Llicència
 
Aquest projecte es distribueix sota la llicència especificada al fitxer [`LICENSE`](./LICENSE).
 
