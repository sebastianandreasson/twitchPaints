import socketIO from "socket.io";
import async from "async";
import moment from "moment";
import _ from "underscore";
import express from "express";
import bodyParser from "body-parser";
import multer from "multer";
import debug_module from "debug";

import chatBot from "./chatbot";
import painter from "./painter";

const debug = debug_module("server");
const debug_socket = debug_module("socket");

var connectedClient;
const app = express();
const io = socketIO.listen(1338);
io.set("log level", 1);

const requestQueue = async.queue((request, callback) => {
    debug(request);
    if (connectedClient){
        connectedClient.emit(request.name, request.payload);
    }
    callback();
});

const sessionQueue = async.queue((request, callback) => {
    if (connectedClient) {
        debug("start session: " + request.name);
        const votingWinner = chatBot.getVotingWinner() ? chatBot.getVotingWinner().name : null;
        switch (request.name) {
            case "endTheme":
                if (votingWinner){
                    request.payload.themeName = votingWinner;
                }
                break;
            case "endNaming":
                if (votingWinner){
                    request.payload.paintingName = votingWinner;
                }
                break;
            default:
                break;
        }
        requestQueue.push(request);

        if (request.chatState) chatBot.setState(request.chatState);
        if (request.chatState && request.chatState === "voting"){
            handleVoting();
        }

        setTimeout(() => {
            debug("end session: " + request.name);
            callback();
        }, request.timeout * 1000);
    }
    else{
        callback();
    }
}, 1);
sessionQueue.drain = () => {
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

let sessions = [
    { name: "startTheme",           chatState: "voting",      payload: { sessionLength: 15 }, timeout: 15 },
    { name: "endTheme",             chatState: null,          payload: { themeName: "dog" }, timeout: 4 },
    { name: "startPainting",        chatState: "painting",    payload: { width: 100, height: 100, sessionLength: 60 }, timeout: 60 },
    { name: "endPainting",          chatState: null,          payload: {}, timeout: 4 },
    { name: "startNaming",          chatState: "voting",      payload: { sessionLength: 15 }, timeout: 15 },
    { name: "endNaming",            chatState: null,          payload: { paintingName: "Twitch Masterpiece" }, timeout: 4 },
];

const randomColor = () => {
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

io.sockets.on("connection", (socket) => {
    debug_socket('socket is connected!');

    socket.on("start", (options) => {
        debug_socket('socket sent start!');
        connectedClient = socket;
        if (options){
            debug_socket(options);
            debug_socket(typeof options);
            async.each(sessions, (session, callback) => {
                debug_socket(session);
                async.each(options, (option, optionCallback) => {
                    debug_socket(option);
                    if (option.name === session.name) {
                        debug("matched, set new sessionLength");
                        session.timeout = option.sessionLength;
                        session.payload.sessionLength = option.sessionLength;
                    }
                    optionCallback();
                }, callback);
            }, () => {
                debug_socket("set all, start");
                debug_socket(sessions);
                sessionQueue.push(sessions);
                chatBot.newStream("sebastaindeveloperaccount");
                chatBot.addListener(requestQueue);
            });
        }
        else{
            debug_socket("got no options, start with default");
            sessionQueue.push(sessions);
            chatBot.newStream("sebastaindeveloperaccount");
            chatBot.addListener(requestQueue);
        }
    });

    socket.on("error", (e) => {
       debug(e);
    });

    socket.on("disconnect", () => {
        debug("CLIENT DISCONNECT");
        if (socket === connectedClient) {
            connectedClient = null;
            chatBot.endStream();
        }
    });

});

app.use(bodyParser());

const storage = multer.diskStorage({
    destination(req, file, cb){
        cb(null, __dirname + "/upload/")
    },
    filename(req, file, cb){
        cb(null, file.originalname);
    }
});
const upload = multer({ dest: __dirname + "/upload", storage: storage }).single("file");

const uploadHandler = (req, res, next) => {
    if (req.url !== '/upload' || req.method !== 'POST') return next()
    upload(req, res, (err) => {
        if (err) return next(err);
    })
}

app.use(uploadHandler);
app.post("/upload", (req, res) => {
    // console.log(req);
    res.send({ code: 200 });
});
app.listen(3000);
