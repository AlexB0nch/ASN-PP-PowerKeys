/* eslint-disable */
const path = require("path");
const webpack = require("webpack");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const CopyWebpackPlugin = require("copy-webpack-plugin");

const DEFAULT_DEV_API = "https://localhost:7168";
const DEFAULT_PROD_API = "https://pptpowerkeys-api.azurewebsites.net";

module.exports = (env, argv) => {
  const dev = argv.mode === "development";
  const apiBaseUrl = (
    process.env.API_BASE_URL || (dev ? DEFAULT_DEV_API : DEFAULT_PROD_API)
  ).replace(/\/$/, "");

  return {
    devtool: dev ? "source-map" : false,
    entry: {
      taskpane: "./src/taskpane/index.tsx",
      commands: "./src/commands/commands.ts",
    },
    output: {
      path: path.resolve(__dirname, "dist"),
      filename: "[name].js",
      publicPath: "./",
      clean: true,
    },
    resolve: {
      extensions: [".ts", ".tsx", ".js"],
    },
    module: {
      rules: [
        {
          test: /\.tsx?$/,
          use: "ts-loader",
          exclude: /node_modules/,
        },
      ],
    },
    plugins: [
      new webpack.DefinePlugin({
        "process.env.API_BASE_URL": JSON.stringify(apiBaseUrl),
      }),
      new HtmlWebpackPlugin({
        filename: "taskpane.html",
        template: "./src/taskpane/taskpane.html",
        chunks: ["taskpane"],
      }),
      new HtmlWebpackPlugin({
        filename: "commands.html",
        template: "./src/commands/commands.html",
        chunks: ["commands"],
      }),
      new CopyWebpackPlugin({
        patterns: [{ from: "assets", to: "assets" }],
      }),
    ],
    devServer: {
      static: { directory: path.join(__dirname, "dist") },
      server: "https",
      port: 3000,
      headers: { "Access-Control-Allow-Origin": "*" },
    },
  };
};
