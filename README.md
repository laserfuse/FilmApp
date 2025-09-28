# FilmApp

FilmApp est une application Windows Forms (.NET 6) qui vous permet d’organiser, prévisualiser et lancer en un clic votre collection de films via une grille de vignettes.

## Fonctionnalités

- Ajout instantané de films (image de couverture + fichier vidéo)  
- Recadrage automatique des vignettes pour un rendu uniforme  
- Scroll vertical exclusif pour naviguer facilement dans la collection  
- Suppression sécurisée avec confirmation  
- Titre et icône personnalisés (logo.ico déjà inclus dans le projet)

## Prérequis

- .NET 6.0 SDK (ou version supérieure)  
- Windows 10 ou 11

## Installation

    git clone https://github.com/votre-utilisateur/FilmApp.git
    cd FilmApp
    dotnet restore
    dotnet run

> Le fichier `logo.ico` est déjà présent à la racine du projet et sera automatiquement copié dans le dossier de sortie.

## Utilisation

1. Cliquez sur **+** pour ajouter un nouveau film (sélectionnez l’image puis la vidéo).  
2. Faites défiler la liste verticalement pour parcourir vos vignettes.  
3. Cliquez sur une vignette pour lancer la vidéo dans votre lecteur par défaut.  
4. Cliquez sur la croix rouge d’une vignette pour supprimer le film de la collection.

## Contribuer

Les contributions sont les bienvenues !  
1. Ouvrez une issue pour proposer une amélioration ou signaler un bug.  
2. Créez une branche à partir de `main` :  
       git checkout -b feature/ma-fonctionnalité  
3. Commitez vos changements :  
       git commit -m "Ajout : nouvelle fonctionnalité"  
4. Poussez votre branche :  
       git push origin feature/ma-fonctionnalité  
5. Ouvrez une pull request sur GitHub.

## Licence

Ce projet est distribué sous licence MIT.  
Voir le fichier [LICENSE](LICENSE) pour plus de détails.
