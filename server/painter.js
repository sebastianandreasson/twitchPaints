var async = require('async'),
    _ = require('underscore'),
    im = require('imagemagick'),
    exec = require('child_process').exec,
    color = require("color"),
    debug = require('debug')('server');

var painter = {};
var chatBot = require('./chatbot.js');

painter.paint = () => {

  var size = 100;
  var grid = [];

  var x = 0;
  var y = 0;
  for (var i = 0; i < size*size; i++) {
    grid.push({
      x: x,
      y: y,
      rgb: {
        r: 0,
        g: 0,
        b: 0
      },
      size: 1
    });
    x++;
    if (x === size){
      x = 0;
      y++;
    }
  }
  async.eachSeries(grid, (coord, callback) => {
    exec("convert ./assets/mona.jpg -crop '1x1+" + coord.x + "+" + coord.y + "' txt:-", {maxBuffer: 1024 * 250}, function(err, stdout, stderr){
      if (err) throw err;
      stdout = stdout.slice(10, stdout.length);
      var hex = stdout.slice(stdout.indexOf("#"), stdout.indexOf("#")+7);
      console.log(hex);
      chatBot.handleChat("paint " + coord.x + " " + coord.y + " " + hex);
      callback();
    });
  }, () => {
  }, 2);
};

module.exports = painter;
