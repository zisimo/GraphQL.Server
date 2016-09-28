import chalk from 'chalk';
import path from 'path';
import settings from './settings';
import { exec } from 'child-process-promise';
import Deferred from './Deferred';

export default function fixie() {
    const runner = path.resolve('./packages/NUnit.ConsoleRunner.3.4.1/tools/nunit3-console.exe');
    const params = `./src/GraphQL.Server.Test/bin/${settings.target}/GraphQL.Server.Test.dll --output ${settings.testOutput}`;

    const deferred = new Deferred();

    exec(`${runner} ${params}`)
      .then(function (result) {
          console.log(chalk.green(result.stdout));
          deferred.resolve();
      })
      .fail(function (err) {
          console.error(chalk.red(err.stdout));
          deferred.reject(err);
      });

    return deferred.promise;
}
