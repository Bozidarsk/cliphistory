mcs *.cs -pkg:gtk-sharp-3.0 -out:cliphistory
chmod 755 ./cliphistory
chown root: ./cliphistory
sudo mv ./cliphistory /usr/local/bin