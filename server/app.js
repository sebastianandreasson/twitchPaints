var io = require('socket.io').listen(1338),
    async = require('async'),
    moment = require('moment'),
    _ = require('underscore'),
    express = require('express'),
    bodyParser = require('body-parser'),
    multer = require('multer'),
    debug_socket = require('debug')('socket');
    debug = require('debug')('server');

var chatBot = require('./chatbot.js');
var painter = require('./painter.js');

var connectedClient;
var app = express();
io.set('log level', 1);

var requestQueue = async.queue(function(request, callback){
    debug(request);
    if (connectedClient){
        connectedClient.emit(request.name, request.payload);
    }
    callback();
});

var sessionQueue = async.queue(function(request, callback){
    if (connectedClient) {
        debug("SESSION: " + request.name);
        if (request.name === "endTheme"){
            request.payload.themeName = chatBot.getVotingWinner().name;
        } else if (request.name === "endNaming"){
            request.payload.paintingName = chatBot.getVotingWinner().name;
        }
        if (request.chatState) chatBot.setState(request.chatState);
        if (request.chatState && request.chatState === "voting"){
            handleVoting();
        }
        requestQueue.push(request);
        setTimeout(callback, request.timeout * 1000);
    }
    else{
        callback();
    }
}, 1);
sessionQueue.drain = function(){
    if (connectedClient){
        debug("drained, reset!");
        sessionQueue.push(sessions);
    }
    else{
        debug("drainged but no client, so do nothing");
    }
};
var handleVoting = function(){
    console.log("handleVoting");
    setTimeout(function(){
        var list;
        if (chatBot.votes){
            list = chatBot.getVotingList();
        }
        if (connectedClient && chatBot.state === "voting"){
            requestQueue.push({ name: "hudList", payload: list ? list : [] });
            handleVoting();
        }
    }, 500);
};

var sessions = [
    { name: "startTheme",           chatState: "voting",      payload: { sessionLength: 15 }, timeout: 15 },
    { name: "endTheme",             chatState: null,          payload: { themeName: "dog" } },
    { name: "startPainting",        chatState: "painting",    payload: { width: 100, height: 100, sessionLength: 60 }, timeout: 60 },
    { name: "endPainting",          chatState: null,          payload: {} },
    { name: "startNaming",          chatState: "voting",      payload: { sessionLength: 15 }, timeout: 15 },
    { name: "endNaming",            chatState: null,          payload: { paintingName: "Twitch Masterpiece" } },
];

randomColor = function(){
    if (connectedClient){
        connectedClient.emit("paint", {
            x: Math.floor(Math.random() * 100) + 1,
            y: Math.floor(Math.random() * 100) + 1,
            rgb: {
                r: Math.floor(Math.random() * 255) + 1,
                g: Math.floor(Math.random() * 255) + 1,
                b: Math.floor(Math.random() * 255) + 1
            },
            size: 1
        });
        setTimeout(function(){
            randomColor();
        }, 1);
    }
};

io.sockets.on('connection', function (socket) {
    debug_socket('socket is connected!');
    if (!connectedClient){
        connectedClient = socket;
        sessionQueue.push(sessions);
        chatBot.newStream("sebastaindeveloperaccount");
        chatBot.addListener(requestQueue);
    }
    // setTimeout(function(){
    //     painter.paint();
    // }, 3000);

    socket.on('error', function(e){
       debug(e);
    });

    socket.on('disconnect', function(){
        debug("CLIENT DISCONNECT");
        if (socket === connectedClient) {
            connectedClient = null;
            chatBot.endStream();
        }
    });

});

app.use(bodyParser());

var storage = multer.diskStorage({
    destination: function (req, file, cb) {
        cb(null, __dirname + '/upload/')
    },
    filename: function (req, file, cb) {
        cb(null, file.originalname);
    }
});
var upload = multer({ dest: __dirname + '/upload', storage: storage }).single('file');

function uploadHandler (req, res, next) {
    if (req.url !== '/upload' || req.method !== 'POST') return next()
    console.log(req.file);
    upload(req, res, function (err) {
        console.log(err);
        if (err) return next(err)
        console.log('UPLOAD FINISHED')
        // console.log(req.files)
        // console.log(req);
    })
}

app.use(uploadHandler);
app.post('/upload', function (req, res) {
    // console.log(req);
    res.send({ code: 200 });
});
app.listen(3000);
