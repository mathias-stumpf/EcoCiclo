'use strict';

require('dotenv').config();
var express = require('express');
var path = require('path');
var session = require('express-session');
var SQLiteStore = require('connect-sqlite3')(session);
var helmet = require('helmet');
var rateLimit = require('express-rate-limit');
var csrf = require('csurf');
var app = express();

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'src', 'views'));
app.use(helmet());
app.use(express.urlencoded({ extended: false, limit: '10kb' }));
app.use(express.static(path.join(__dirname, 'public')));
app.use(session({
  store: new SQLiteStore({ db: 'sessions.sqlite', dir: path.join(__dirname, 'database') }),
  secret: process.env.SESSION_SECRET || 'development-only-change-this-secret',
  resave: false,
  saveUninitialized: false,
  cookie: { httpOnly: true, sameSite: 'lax', secure: process.env.NODE_ENV === 'production', maxAge: 86400000 }
}));
app.use(csrf());
app.use(function (req, res, next) { res.locals.csrfToken = req.csrfToken(); next(); });
app.use('/', require('./src/routes/index'));
app.use(function (req, res) { res.status(404).render('404', { title: 'Página não encontrada' }); });
app.use(function (err, req, res, next) {
  if (err.code === 'EBADCSRFTOKEN') return res.status(403).send('Formulário expirado. Volte e tente novamente.');
  console.error(err.message);
  res.status(500).render('500', { title: 'Erro interno' });
});

var port = Number(process.env.PORT) || 3000;
app.listen(port, function () { console.log('EcoCiclo disponível na porta ' + port); });
