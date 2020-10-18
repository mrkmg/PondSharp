const devConfig = require("./webpack.config");

devConfig.devtool = false;
devConfig.optimization.minimize = true;

module.exports = devConfig;