var ig = require('instagram-node').instagram(),
    debug = require('debug')('insta_bot');

require('dotenv').load();
var instaBot = {};

var fakeImages = [
    'https://scontent.cdninstagram.com/hphotos-xpa1/t51.2885-15/e35/12071168_1624017604552766_118619584_n.jpg',
    'https://scontent.cdninstagram.com/hphotos-xaf1/t51.2885-15/e35/10375886_1543065202601995_272120877_n.jpg',
    'https://scontent.cdninstagram.com/hphotos-xap1/t51.2885-15/e35/11909271_1813408702218784_1156302726_n.jpg'
];

ig.use({
  client_id: process.env.INSTAGRAM_CLIENTID,
  client_secret: process.env.INSTAGRAM_CLIENTSECRET
});
var auth = function(){
    var url = ig.get_authorization_url("http://google.com", { scope: ['media'], state: 'state' });
    console.log(url);

    ig.authorize_user("code", url, function(err, result) {
        if (err) {
            console.log(err);
            console.log(err.body);
        }
        else {
            console.log(result);
            console.log('Yay! Access token is ' + result.access_token);
            ig.use({ access_token: result.access_token });
        }
    });
};

var sendImage = function(){
    setTimeout(function(){
        instaBot.send({
            type: "instagram",
            instagram: {
                username: "bob",
                imageURL: fakeImages[Math.floor(Math.random() * fakeImages.length)]
            }
        });
        if (instaBot.active){
            sendImage();
        }
    }, 5000);
};

instaBot.newStream = function(hashtag){
    instaBot.active = true;
    sendImage();
};

instaBot.endStream = function(){
    debug('endStream');
    instaBot.active = false;
};

instaBot.addListener = function(requestQueue){
    instaBot.send = requestQueue.push;
};

module.exports = instaBot;
