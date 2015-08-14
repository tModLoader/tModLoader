import sys
import difflib

baselines = open(sys.argv[1], 'U').readlines()
srclines = open(sys.argv[2], 'U').readlines()

#strip unicode BOM
if baselines[0].startswith('\xef\xbb\xbf'):
    baselines[0] = baselines[0][3:]
if srclines[0].startswith('\xef\xbb\xbf'):
    srclines[0] = srclines[0][3:]

print ''.join(difflib.unified_diff(baselines, srclines, sys.argv[3], sys.argv[4], '', '', n=3))
