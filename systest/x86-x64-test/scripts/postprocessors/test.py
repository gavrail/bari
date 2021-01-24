import os

print 'Target dir:' + targetDir

genExe = get_tool("fsrepo://MessageGenerator/*.*", "MessageGenerator.exe")

cmd = genExe + ' "' + targetDir+os.path.sep+'generated.txt" "%s"' % ",".join(
    ["Hello_world", targetGoalName] + list(targetIncorporatedGoalsNames))

if is_mono:
    cmd  = 'mono ' + cmd


print cmd
os.system(cmd)

results = ['generated.txt']
