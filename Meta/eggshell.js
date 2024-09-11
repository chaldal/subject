const eggshellCli = require("./AppEggshellCli/dist/index")

eggshellCli.run()
.catch(e => {
  console.log(e);
  process.exit(-1);
})
.then(() => process.exit(0))
