solution_file = "src/Wia.sln"
configuration = "release"
version = env("version")
build_name = ""

if version != null:
  build_name = "WIA-" + version
else:
  build_name = "WIA"

build_dir = "build/${build_name}"

target default, (init, compile, deploy, package):
  pass

target init:
  rmdir(build_dir)
  
desc "Compiles the solution"
target compile:
  msbuild(file: solution_file, configuration: configuration)
  
desc "Copies the binaries to the 'build' directory"
target deploy:
  print "Copying to build dir"

  with FileList("src/WIA/bin/${configuration}"):
    .Include("*.{exe}")
    .Exclude("*.vshost.exe")
    .ForEach def(file):
      file.CopyToDirectory(build_dir)
  
  print "Copying docs files to build dir"
  
  with FileList("docs"):
    .Include("*.{reg}")
    .ForEach def(file):
      file.CopyToDirectory(build_dir)
      
  print "Copy readme file to build dir"
  
  cp("README.md", build_dir + "/README.txt")
      
desc "Creates zip package"
target package:
  zip(build_dir, build_dir + '.zip')
