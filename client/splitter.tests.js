module.exports = {
    entry: "tests/Client.Tests.fsproj",
    outDir: "tests-js",
    fable: { define: ["MOCHA_TESTS"] },
    babel: {
        sourceMaps: false,
        presets: [
            ["@babel/preset-react"],
            ["@babel/preset-env", { modules: "commonjs" }]
        ]
    }
}