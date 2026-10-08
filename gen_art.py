from PIL import Image
# gun crop from the red man image -> item art
img = Image.open("gen/redman_cut.png").convert("RGBA")
gun = img.crop((200, 195, 360, 270))
gun.save("gen/pistol_cut.png")
# walking sheet for the red man: facing left, 4 frames, legs sway
box = img.getbbox(); man = img.crop(box)
H = 54
k = H / man.height
man = man.resize((round(man.width*k), H), Image.Resampling.BOX)
px = man.load()
for y in range(man.height):
    for x in range(man.width):
        r,g,b,a = px[x,y]; px[x,y] = (r,g,b,255 if a>=128 else 0)
w,h = man.size
hip = int(h*0.52)
amps = [3, 0, -3, 0]; bob = [0, 1, 0, 1]
fw, fh = w+8, h+2
sheet = Image.new("RGBA", (fw, fh*4), (0,0,0,0))
for i in range(4):
    fr = Image.new("RGBA", (fw, fh), (0,0,0,0))
    upper = man.crop((0,0,w,hip)); fr.paste(upper, (4, 1+bob[i]), upper)
    for y in range(hip, h):
        t = (y-hip)/(h-hip)
        row = man.crop((0,y,w,y+1))
        fr.paste(row, (4+int(round(amps[i]*t)), 1+y+bob[i]), row)
    sheet.paste(fr, (0, i*fh))
sheet.save("mod/RedMan.png")
print("RedMan frame", fw, fh)
