import { exec, rm } from 'shelljs';
import Deferred from './Deferred';
import settings from './settings';

export default function compile() {
  const deferred = new Deferred();
  rm('-rf', `GraphQL.Server/obj`);
  rm('-rf', `GraphQL.Server/bin`);
  exec(`dotnet build src/GraphQL.Server.Tests -c ${settings.target}`, (code, stdout, stderr)=> {
    if(code === 0) {
      deferred.resolve();
    } else {
      deferred.reject(stderr);
    }
  });
  return deferred.promise;
}
