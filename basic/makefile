# Makefile for https://github.com/Threetwosevensixseven/NextLayer2Examples
# Only tested with GNU MAKE 3.81 for Windows.
# If you fix this up for linux or mac, please contribute your changes.

# Change these variables to suit your own environment
BASICDIR = .
HDFMONKEY = ..\tools\hdfmonkey
EMUDIR = C:\spec\CSpect2_19_4_4
EMUSD = C:\spec\sd208\cspect-next-2gb.img
SYNCDIR = C:\spec\sync\ON
RM = del /F /Q
MD = md
CP = copy /YB 
KILL = taskkill /F /IM

# Default target is to build the dot command from source

# Builds, injects built dot and autoexec into CSpect SD image, and starts CSpect asynchronously.
emu:
	-$(HDFMONKEY) mkdir $(EMUSD) NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\bridge.pal NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\bridge1.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\bridge2.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\bridge3.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\bridge4.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\bridge5.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\watch.pal NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\watch1.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\watch2.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)\watch3.bin NextLayer2Examples
	$(HDFMONKEY) put $(EMUSD) $(BASICDIR)/autoexec.bas NextZXOS
#   Uncomment this to get the current autoexec.bas in the image back into the project:
	$(HDFMONKEY) get $(EMUSD) /NextZXOS/autoexec.bas $(BASICDIR)/autoexec.bas
	-$(KILL) cspect.exe 
	CMD /C start /d $(EMUDIR) cspect -w3 -zxnext -nextrom -basickeys -exit -brk -tv -emu -mmc=$(EMUSD)

# Builds, copies dot and autoexec to NextSync sync directory, and starts starts the server asynchronously.
# WHen you F4, autoexec will skip running .fastsync (because CSpect defaults to core version 4.0.0), then run the dot command.
# The first time you make sync, run .fastsync manually to get the files.
sync:
	-$(MD) "$(SYNCDIR)\NextLayer2Examples"
	copy "$(BASICDIR)\*.bin" "$(SYNCDIR)\NextLayer2Examples\*.*"
	copy "$(BASICDIR)\*.pal" "$(SYNCDIR)\NextLayer2Examples\*.*"
	-$(MD) "$(SYNCDIR)\NextZXOS"
	copy "$(BASICDIR)\autoexec.bas" "$(SYNCDIR)\NextZXOS\*.*"             

# Warning: kills all running python processes
# Remove these two lines and start the server manually if that bothers you
	-$(KILL) py.exe
	CMD /C start /d $(SYNCDIR) /min nextsync.py
