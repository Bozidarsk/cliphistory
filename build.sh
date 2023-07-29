# build: mono, gtk-sharp-3
# runtime: mono, gtk-sharp-3, gtk-layer-shell, wl-clipboard

mcs *.cs -pkg:gtk-sharp-3.0 -out:cliphistory
chmod 755 ./cliphistory
sudo chown root:root ./cliphistory
sudo mv ./cliphistory /usr/local/bin
