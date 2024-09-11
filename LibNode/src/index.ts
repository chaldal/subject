// Note let's use the `Plus` suffix for all extensions to existing
// node modules, so as to have consistency and avoid collisions with
// said modules, as well as to avoid inviting users to invent their
// own aliases.

import * as fsPlus from "./fs"
export { fsPlus }

import * as childProcessPlus from "./childProcess"
export { childProcessPlus }
