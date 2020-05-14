const Discord = require('discord.js');
const client = new Discord.Client();
const config = require('./config.json');
const net = require('net');
const fs = require('fs');

var database = "{\"players\":{}}";

if (fs.existsSync('./data.json'))
    database = fs.readFileSync('./data.json')

var jsondatabase = JSON.parse(database);

saveDataBase();

function saveDataBase() {
    fs.writeFileSync('./data.json', JSON.stringify(jsondatabase));
}

var stdin = process.openStdin();

stdin.addListener('data', function (d) {

});

client.on('ready', () => {
    console.log(`Logged in as ${client.user.tag}!`);
});

var clients = [];

client.on('message', msg => {
    if (msg.content.startsWith(config.prefix) && msg.channel.id === config.channel) {
        var args = msg.content.split(' ');
        switch (args[0]) { // command
            case config.prefix + 'sync':
                console.log(args[1]);
                if (args.length === 2) {
                    jsondatabase.players[msg.author.id] = args[1];
                    saveDataBase();
                    msg.reply('```diff\n+ Steam64Id Connected```');
                }
                else {
                    msg.reply('```diff\n- Missing Steam64Id```');
                }
                break;
            case config.prefix + 'players':
            case config.prefix + 'list':
                for (var i = 0; i < clients.length; i++) {
                    var sendme = {
                        'command': 'playerlist'
                    };
                    clients[i].write(JSON.stringify(sendme) + ';');
                }
                break;
            default:
                msg.reply('```diff\n- Invalid Command```');
                break;
        }
    }
});

client.login(config.token);

var server = net.createServer(function (sock) {
    console.log('OPENED: IP=' + sock.remoteAddress + ' PORT=' + sock.remotePort);
    clients.push(sock);
    sock.on('data', function (data) {
        try {
            var jdata = JSON.parse(data.toString());
            console.log(jdata);
            switch (jdata.command) {
                case 'playerjoin':
                    {
                        var keys = Object.keys(jsondatabase.players);
                        var objs = Object.values(jsondatabase.players);
                        for (var i = 0; i < objs.length; i++) {
                            if (objs[i] === jdata.steamid) { //steamid check


                                var guild = client.guilds.resolve(config.guild);
                                var mem = guild.member(keys[i]);
                                for (var j = 0; j < config.roles.length; j++) { // get all roles
                                    var role = mem.roles.cache.get(config.roles[j].id);
                                    if (role !== undefined) { // has role check

                                        var sendme = {
                                            'command': 'rolesync',
                                            'role': config.roles[j].ign,
                                            'id': jdata.steamid
                                        };
                                        sock.write(JSON.stringify(sendme) + ';');
                                    }
                                }


                            }
                        }
                    }
                    break;
                case 'playerlist':
                    {
                        var list = jdata.players;
                        var str = '```\n';
                        for (var k = 0; k < list.length; k++) {
                            str += list[k] + '\n';
                        }
                        str += '```';
                        var listguild = client.guilds.resolve(config.guild);
                        var chnl = listguild.channels.resolve(config.channel);
                        chnl.send('List of players: ' + str);
                    }
                    break;
            }
        }
        catch (e) {
            console.log(e);
        }
    });

    sock.on('close', function (data) {
        console.log('CLOSED: IP=' + sock.remoteAddress + ' PORT=' + sock.remotePort);
        clients.splice(clients.indexOf(sock), 1);
    });
});

server.listen(config.port, function () {
    console.log('LISTEN on port ' + config.port);
});