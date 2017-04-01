import socketIO from "socket.io";
import async from "async";
import _ from "underscore";
import express from "express";
import bodyParser from "body-parser";
import multer from "multer";
import debug_module from "debug";

import chatBot from "./chatbot";

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
    chatBot.newStream("twitchplace");
    chatBot.addListener(requestQueue);
  });

  socket.on("error", error => debug(error));

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
