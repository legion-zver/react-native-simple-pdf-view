# react-native-simple-pdf-view

## Getting started

`$ npm install react-native-simple-pdf-view --save`

### Mostly automatic installation

`$ react-native link react-native-simple-pdf-view`

### Manual installation


#### iOS

1. In XCode, in the project navigator, right click `Libraries` ➜ `Add Files to [your project's name]`
2. Go to `node_modules` ➜ `react-native-simple-pdf-view` and add `RNSimplePdfView.xcodeproj`
3. In XCode, in the project navigator, select your project. Add `libRNSimplePdfView.a` to your project's `Build Phases` ➜ `Link Binary With Libraries`
4. Run your project (`Cmd+R`)<

#### Android

1. Open up `android/app/src/main/java/[...]/MainApplication.java`
  - Add `import com.reactlibrary.RNSimplePdfViewPackage;` to the imports at the top of the file
  - Add `new RNSimplePdfViewPackage()` to the list returned by the `getPackages()` method
2. Append the following lines to `android/settings.gradle`:
  	```
  	include ':react-native-simple-pdf-view'
  	project(':react-native-simple-pdf-view').projectDir = new File(rootProject.projectDir, 	'../node_modules/react-native-simple-pdf-view/android')
  	```
3. Insert the following lines inside the dependencies block in `android/app/build.gradle`:
  	```
      compile project(':react-native-simple-pdf-view')
  	```

#### Windows
[Read it! :D](https://github.com/ReactWindows/react-native)

1. In Visual Studio add the `RNSimplePdfView.sln` in `node_modules/react-native-simple-pdf-view/windows/RNSimplePdfView.sln` folder to their solution, reference from their app.
2. Open up your `MainPage.cs` app
  - Add `using Simple.Pdf.View.RNSimplePdfView;` to the usings at the top of the file
  - Add `new RNSimplePdfViewPackage()` to the `List<IReactPackage>` returned by the `Packages` method


## Usage
```javascript
import RNSimplePdfView from 'react-native-simple-pdf-view';

// TODO: What to do with the module?
RNSimplePdfView;
```
