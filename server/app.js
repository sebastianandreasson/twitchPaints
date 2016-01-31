var io = require('socket.io').listen(1338),
    async = require('async'),
    moment = require('moment'),
    _ = require('underscore'),
    express = require('express'),
    bodyParser = require('body-parser'),
    debug_socket = require('debug')('socket');
    debug = require('debug')('server');

var app = express();
io.set('log level', 1);

var requestQueue = async.queue(function(operation, callback){

});

var chatBot = require('./chatbot.js');
chatBot.addListener(requestQueue);

io.sockets.on('connection', function (socket) {
    connectedClient = socket;
    debug_socket('socket is connected!');

    socket.on('webserverConnect', function(){
        debug_socket('webserver connected!');
    });

    socket.on('initClient', function(){
        debug_socket('client connected');
        // connectedClient = socket;
    });

    socket.on('error', function(e){
       debug(e);
    });

    socket.on('disconnect', function(){
        if (socket === connectedClient){
            connectedClient = null;
            if (chatBot) chatBot.endStream();
        }
    });

});

app.use(bodyParser());
app.listen(3000);
