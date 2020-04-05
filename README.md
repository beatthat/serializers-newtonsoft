## Install

From your unity project folder:

    npm init
    npm install {package.scope}/{package.name} --save

## Development

You can edit the code and samples in the test environment and then use ```npm run test-install``` to sync changes back to the package src.

```
    npm run test-install
    cd test

    # edit code under Assets/Plugins/packages/{package.scope}/{package.name}
    # edit samples under Assets/Samples/packages/{package.scope}/{package.name}

    # sync changes back to src
    npm run test-overwrite2src
```

**REMEMBER:** changes made under the test folder are not saved to the package
unless they are copied back into the source folder
