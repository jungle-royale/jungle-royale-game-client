/Applications/Unity/Hub/Editor/2022.3.55f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -quit \
  -projectPath . \
  -executeMethod WebGLBuildScript.BuildWebGL \

echo '---------- 🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎 -------------'

rm -rf server/Builds

mv Builds ./server
