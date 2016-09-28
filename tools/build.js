import make from './make';
import {
  artifacts,
  compile,
  dotnetPack,
  dotnetTest,
  nunit,
  restore,
  setVersion,
  appVeyorVersion,
  version
} from './tasks';

const args = process.argv.slice(2);

const tasks = {
  artifacts: ['nuget', artifacts],
  compile: ['restore', compile],
  test: nunit,
  appVeyorVersion,
  version: [version, appVeyorVersion],
  nuget: dotnetPack,
  restore,
  setVersion: () => setVersion(args[1]),
  'default': 'compile test',
  ci: 'version default artifacts'
};

make(tasks);
