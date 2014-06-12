Baixar na pasta C:\Users\<usuário>\Documents\Visual Studio 2010\Projects\VSMacros80 e carregar no Visual Studio pelo menu "Tools->Macros->Load Macro Project...".

As macros devem estar em UTF16. Entretanto, o GIT diff entende como binário arquivos desse tipo. Para fazer o GIT reconhecer os scripts como texto, como também ter o side-by-side no Gerrit, deve-se incluir no ~/.gitconfig o filtro abaixo para comitar sempre como UTF8 (Obs: O TortoiseGit não funciona com filtros na hora de comitar arquivos adicionados).

[filter "utf16"]
   clean = iconv -f utf-16le -t utf-8
   smudge = iconv -f utf-8 -t utf-16le
   required
