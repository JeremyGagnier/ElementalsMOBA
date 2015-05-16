import pygame

pygame.init()

screen = pygame.display.set_mode((1,1))

x = pygame.Surface((255 * 5, 5), pygame.SRCALPHA)

for i in range(0, 256):
    #pygame.draw.rect(x, (0, 0, 0, i), (5 * i, 0, 5, 5))
    pygame.draw.rect(x, (0, 0, 0, min(255, round(((i)**0.5)*16))), (5 * i, 0, 5, 5))
    #pygame.draw.rect(x, (0, 0, 0, min(255, round(((i)**0.25)*64))), (5 * i, 0, 5, 5))
    #pygame.draw.rect(x, (0, 0, 0, min(255, round(((i)**2)/16))), (5 * i, 0, 5, 5))

pygame.image.save(x, "../Art/AlphaGradiant.png");


quit()
