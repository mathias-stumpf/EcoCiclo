'use strict';
var express = require('express');
var router = express.Router();
var searchLimiter = require('express-rate-limit')({ windowMs: 60000, max: 60 });

router.get('/', function (req, res) {
  res.render('home', { title: 'EcoCiclo | Descarte consciente', query: '', results: null });
});
router.get('/residuos/buscar', searchLimiter, function (req, res) {
  var query = typeof req.query.q === 'string' ? req.query.q.trim().replace(/\s+/g, ' ').slice(0, 100) : '';
  if (!query) return res.status(400).render('home', { title: 'EcoCiclo | Descarte consciente', query: '', results: [], message: 'Digite o nome de um resíduo para pesquisar.' });
  res.render('home', { title: 'Resultados da busca | EcoCiclo', query: query, results: [], message: 'Ainda não há instruções cadastradas para esse termo.' });
});
module.exports = router;
