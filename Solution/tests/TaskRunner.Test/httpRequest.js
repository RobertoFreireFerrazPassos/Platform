module.exports = async (params) => {
    var http = require('http');

    return new Promise(function (resolve, reject) {
        var req = http.request(params, function (res) {
            // reject on bad status
            if (res.statusCode < 200 || res.statusCode >= 300) {
                return reject(new Error('statusCode=' + res.statusCode));
            }
            // cumulate data
            var body = [];
            res.on('data', function (chunk) {
                body.push(chunk);
            });
            // resolve on end
            res.on('end', function () {
                try {
                    body = JSON.parse(Buffer.concat(body).toString());
                } catch (e) {
                    reject(e);
                }
                resolve(body);
            });
        });

        if (['POST', 'PUT', 'PATCH'].includes(params.method))
        {
            req.write(JSON.stringify(params.body));
        }

        // reject on request error
        req.on('error', function (err) {
            reject(err);
        });
        // IMPORTANT
        req.end();
    });
}