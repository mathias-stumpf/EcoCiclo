using EcoCiclo.Data;
using EcoCiclo.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoCiclo.Services;

public static class InicializadorBancoDados
{
    private const string FonteMmaReciclagem = "https://www.gov.br/mma/pt-br/assuntos/educacao-ambiental/glossario-ea/o-que-e-reciclagem";
    private const string FonteMmaCompostagem = "https://www.gov.br/mma/pt-br/assuntos/meio-ambiente-urbano-recursos-hidricos-qualidade-ambiental/residuos-solidos-urbanos/compostagem_manual_2018_11_26_digital_figuras_c_titulo.pdf";
    private const string FonteSinirPilhas = "https://www.sinir.gov.br/perfis/logistica-reversa/logistica-reversa/pilhas-e-baterias/";
    private const string FontePrefeituraOleo = "https://www.garca.sp.gov.br/coleta-de-oleo-de-cozinha";
    private const string FontePrefeituraVidro = "https://vitoria.es.gov.br/noticias/saiba-o-que-pode-e-o-que-nao-pode-ser-descartado-na-coleta-de-vidros-57021";

    public static async Task InicializarAsync(EcoCicloDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var categoriasIniciais = new[]
        {
            NovaCategoria("Plásticos", "plasticos", "Embalagens e outros itens plásticos. A aceitação depende da coleta seletiva local.", "#4a9b70"),
            NovaCategoria("Papel e papelão", "papel-e-papelao", "Papéis e embalagens de papel ou papelão, quando limpos e secos.", "#b9984f"),
            NovaCategoria("Metais", "metais", "Latas e outros itens metálicos. A aceitação depende da coleta seletiva local.", "#718b91"),
            NovaCategoria("Vidros", "vidros", "Embalagens de vidro e cuidados para evitar acidentes.", "#648c68"),
            NovaCategoria("Óleo de cozinha", "oleo-de-cozinha", "Óleo de cozinha usado, armazenado para encaminhamento adequado.", "#c39c45"),
            NovaCategoria("Pilhas e baterias", "pilhas-e-baterias", "Itens que devem seguir os canais de logística reversa.", "#8a7c68"),
            NovaCategoria("Resíduos orgânicos", "residuos-organicos", "Restos de alimentos e outros materiais orgânicos.", "#77904e")
        };

        foreach (var categoria in categoriasIniciais)
        {
            if (!await dbContext.Categorias.AnyAsync(item => item.Slug == categoria.Slug, cancellationToken))
            {
                dbContext.Categorias.Add(categoria);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var categorias = await dbContext.Categorias.ToDictionaryAsync(item => item.Slug, cancellationToken);
        var residuosIniciais = CriarResiduosIniciais(categorias);

        foreach (var residuo in residuosIniciais)
        {
            var existente = await dbContext.Residuos
                .Include(item => item.Sinonimos)
                .SingleOrDefaultAsync(item => item.Slug == residuo.Slug, cancellationToken);

            if (existente == null)
            {
                dbContext.Residuos.Add(residuo);
                continue;
            }

            var sinonimosExistentes = existente.Sinonimos.Select(item => item.NomeNormalizado).ToHashSet(StringComparer.Ordinal);
            foreach (var sinonimo in residuo.Sinonimos.Where(item => !sinonimosExistentes.Contains(item.NomeNormalizado)))
            {
                existente.Sinonimos.Add(sinonimo);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await PopularCatalogoSolicitadoAsync(dbContext, cancellationToken);
    }

    private static async Task PopularCatalogoSolicitadoAsync(EcoCicloDbContext dbContext, CancellationToken cancellationToken)
    {
        var categoriasEsperadas = new Dictionary<int, string>
        {
            [1] = "Plásticos",
            [2] = "Papel e papelão",
            [3] = "Metais",
            [4] = "Vidros",
            [5] = "Óleo de cozinha",
            [6] = "Pilhas e baterias",
            [7] = "Resíduos orgânicos"
        };

        var categorias = await dbContext.Categorias
            .Where(categoria => categoriasEsperadas.Keys.Contains(categoria.Id))
            .ToDictionaryAsync(categoria => categoria.Id, cancellationToken);

        foreach (var (id, nome) in categoriasEsperadas)
        {
            if (!categorias.TryGetValue(id, out var categoria) || categoria.Nome != nome)
            {
                throw new InvalidOperationException(
                    $"A categoria esperada '{nome}' não foi encontrada com ID {id}. Os resíduos do catálogo não foram inseridos.");
            }
        }

        var residuos = CriarCatalogoSolicitado(categorias);
        var slugs = residuos.Select(residuo => residuo.Slug).ToArray();
        var nomesNormalizados = residuos.Select(residuo => residuo.NomeNormalizado).ToArray();
        var existentes = await dbContext.Residuos
            .Where(residuo => slugs.Contains(residuo.Slug) || nomesNormalizados.Contains(residuo.NomeNormalizado))
            .Select(residuo => new { residuo.Slug, residuo.NomeNormalizado, residuo.CategoriaId })
            .ToListAsync(cancellationToken);

        var slugsExistentes = existentes.Select(item => item.Slug).ToHashSet(StringComparer.Ordinal);
        var nomesExistentes = existentes
            .Select(item => (item.CategoriaId, item.NomeNormalizado))
            .ToHashSet();

        foreach (var residuo in residuos)
        {
            if (slugsExistentes.Contains(residuo.Slug) ||
                nomesExistentes.Contains((residuo.CategoriaId, residuo.NomeNormalizado)))
            {
                continue;
            }

            dbContext.Residuos.Add(residuo);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<Residuo> CriarCatalogoSolicitado(IReadOnlyDictionary<int, Categoria> categorias)
    {
        return new List<Residuo>
        {
            Item("Garrafa PET", "garrafa-pet", 1, "Embalagem plástica para bebidas, geralmente identificada pelo código PET.", "Esvazie, retire o excesso de resíduos e, se necessário, enxágue rapidamente e deixe secar.", "Encaminhe à coleta seletiva local se aceita; consulte calendário e regras do município.", "Não coloque com líquido ou restos de alimento.", "Amasse somente se isso for permitido e seguro na coleta local.", FonteMmaReciclagem),
            Item("Pote de plástico", "pote-de-plastico", 1, "Recipiente plástico usado para armazenar alimentos ou outros produtos.", "Esvazie, retire restos e deixe limpo e seco.", "Separe para coleta seletiva conforme as regras municipais; confirme se o tipo de plástico é aceito.", "Não descarte com conteúdo ou resíduos perigosos.", "Evite usar água em excesso na lavagem.", FonteMmaReciclagem),
            Item("Embalagem de produto de limpeza", "embalagem-produto-limpeza", 1, "Frasco ou embalagem plástica que continha produto de limpeza doméstica.", "Use o produto até o fim e esvazie a embalagem; enxágue apenas se o rótulo e as orientações locais permitirem.", "Encaminhe à coleta seletiva se a embalagem estiver vazia e o serviço local a aceitar.", "Não misture produtos químicos nem reutilize para guardar alimentos.", "Siga o rótulo do produto e evite contato com resíduos concentrados.", FonteMmaReciclagem),
            Item("Caixa de papelão", "caixa-de-papelao", 2, "Embalagem de papelão que pode ser reciclável quando limpa e seca.", "Retire conteúdos, fitas e excesso de materiais; achate a caixa e mantenha-a seca.", "Separe para coleta seletiva conforme calendário e regras municipais.", "Não encaminhe papelão molhado ou engordurado junto com papel limpo sem confirmar a regra local.", "Cuidado com grampos, pregos e bordas ao desmontar.", FonteMmaReciclagem),
            Item("Jornal usado", "jornal-usado", 2, "Papel impresso de jornal, reciclável quando limpo e seco e aceito localmente.", "Mantenha seco e retire resíduos não recicláveis presos às folhas.", "Encaminhe à coleta seletiva de papel conforme as regras do município.", "Não misture com papel sanitário, molhado ou contaminado.", "Armazene em local seco até a coleta.", FonteMmaReciclagem),
            Item("Folhas de papel", "folhas-de-papel", 2, "Folhas de papel de escritório ou escolar sem contaminação relevante.", "Reúna as folhas secas e retire clipes ou espirais quando possível.", "Separe para a coleta seletiva local; consulte os materiais aceitos.", "Não misture com papel engordurado, molhado ou sanitário.", "Destruição de documentos deve considerar a privacidade antes do descarte.", FonteMmaReciclagem),
            Item("Lata de alumínio", "lata-de-aluminio", 3, "Embalagem metálica de bebida ou alimento feita de alumínio.", "Esvazie e retire o excesso de resíduos; uma passada de água basta quando necessária.", "Separe para a coleta seletiva local se aceita; confirme calendário e regras municipais.", "Não descarte com líquido ou restos de alimentos.", "A borda pode cortar; manuseie com cuidado.", FonteMmaReciclagem),
            Item("Lata de aço", "lata-de-aco", 3, "Embalagem metálica de aço usada para alimentos ou outros produtos.", "Esvazie, remova excesso de resíduos e deixe escorrer; limpe apenas o necessário.", "Encaminhe à coleta seletiva se o município aceitar embalagens de aço.", "Não descarte com conteúdo ou bordas expostas sem cuidado.", "A tampa e as bordas podem ser cortantes.", FonteMmaReciclagem),
            Item("Papel-alumínio limpo", "papel-aluminio-limpo", 3, "Folha fina de alumínio sem resíduos de alimentos ou gordura.", "Junte os pedaços limpos em uma bolinha para facilitar o manuseio.", "Verifique se a coleta seletiva local aceita papel-alumínio; encaminhe conforme as orientações municipais.", "Não inclua papel sujo de gordura ou com restos de comida como reciclável limpo.", "Evite deixar pedaços soltos que possam se espalhar.", FonteMmaReciclagem),
            Item("Garrafa de vidro", "garrafa-de-vidro", 4, "Embalagem de vidro; outros objetos de vidro podem exigir destinação diferente.", "Esvazie e retire o excesso de resíduos, mantendo a peça inteira sempre que possível.", "Entregue à coleta seletiva somente se o município aceitar esse tipo de vidro.", "Não misture lâmpadas, espelhos, louças ou vidro temperado sem orientação específica.", "Se quebrar, recolha sem contato direto e acondicione em embalagem rígida identificada.", FontePrefeituraVidro),
            Item("Pote de conserva de vidro", "pote-conserva-vidro", 4, "Pote de vidro usado para conservas e alimentos.", "Esvazie e retire o excesso de resíduos; a lavagem não precisa consumir muita água.", "Separe para coleta ou ponto de entrega que aceite embalagens de vidro na localidade.", "Não misture a tampa metálica se a coleta pedir separação de materiais.", "Mantenha o pote inteiro e protegido até a entrega.", FontePrefeituraVidro),
            Item("Caco de vidro", "caco-de-vidro", 4, "Fragmentos de vidro que oferecem risco de cortes e podem não ser aceitos na coleta comum de recicláveis.", "Sem tocar diretamente, coloque os cacos em recipiente rígido ou embrulhe em material resistente e feche bem.", "Siga a orientação municipal para vidro quebrado; identifique claramente o volume antes da coleta ou entrega.", "Não coloque cacos soltos em sacola fina nem misture sem proteção ao reciclável comum.", "Use proteção adequada ao manusear e mantenha longe de crianças e animais.", FontePrefeituraVidro),
            Item("Óleo de fritura usado", "oleo-fritura-usado", 5, "Óleo vegetal que sobrou após frituras e não deve ser despejado no esgoto.", "Espere esfriar completamente e transfira para garrafa resistente, limpa e bem fechada.", "Leve a um ponto de coleta confirmado na localidade; disponibilidade e critérios variam por município.", "Não despeje na pia, no vaso sanitário, no solo ou em cursos d'água.", "Não transfira quente; evite encher recipientes frágeis ou com vazamento.", FontePrefeituraOleo),
            Item("Óleo vegetal usado", "oleo-vegetal-usado", 5, "Óleo vegetal usado no preparo de alimentos, incluindo óleo de cozinha residual.", "Deixe esfriar totalmente e armazene em recipiente resistente, fechado e sem misturar água ou outros líquidos.", "Entregue em ponto de coleta que confirme receber óleo usado; consulte canais oficiais locais.", "Não descarte na rede de esgoto nem no lixo junto com recicláveis.", "Guarde longe de calor e fora do alcance de crianças.", FontePrefeituraOleo),
            Item("Óleo de cozinha armazenado", "oleo-cozinha-armazenado", 5, "Óleo de cozinha usado já acondicionado para aguardar o encaminhamento correto.", "Mantenha em garrafa resistente, bem tampada e identificada, depois de o óleo esfriar.", "Entregue a ponto de coleta confirmado. Não há calendário ou local universal; verifique a informação do município.", "Não despeje o conteúdo nem coloque a garrafa em coleta seletiva de embalagens sem confirmação.", "Confira se o recipiente está íntegro e sem vazamentos.", FontePrefeituraOleo),
            Item("Pilha alcalina", "pilha-alcalina", 6, "Pilha de uso doméstico que deve seguir o sistema de logística reversa de pilhas e baterias.", "Mantenha seca e guarde temporariamente em recipiente protegido; preserve os polos e a carcaça.", "Entregue em ponto de recebimento específico disponibilizado por comerciantes, fabricantes ou canais oficiais de logística reversa; confirme o recebimento antes de ir.", "Não descarte no lixo comum nem na coleta seletiva de embalagens.", "Não abra, perfure, queime ou tente recarregar uma pilha não recarregável; evite contato com pilha vazando.", FonteSinirPilhas),
            Item("Bateria de celular", "bateria-de-celular", 6, "Bateria portátil de celular, sujeita à logística reversa específica.", "Se estiver íntegra, mantenha protegida contra umidade e impactos; isole os terminais expostos com fita não condutiva se for seguro fazê-lo.", "Leve a ponto de recebimento específico de baterias ou logística reversa indicado por fabricante ou comerciante; confirme previamente.", "Não descarte no lixo comum, não perfure, esmague, incinere nem tente abrir.", "Se estiver estufada, quente, danificada ou vazando, não manuseie além do necessário; afaste de materiais combustíveis e peça orientação ao serviço local.", FonteSinirPilhas),
            Item("Pilha recarregável", "pilha-recarregavel", 6, "Pilha projetada para recarga, que também deve retornar a um canal de recebimento apropriado ao fim da vida útil.", "Mantenha seca, sem danos, e armazene temporariamente separada de objetos metálicos soltos.", "Entregue em ponto de recebimento específico de pilhas e baterias informado por comerciante, fabricante ou canal oficial; confirme antes.", "Não descarte no lixo comum ou junto a recicláveis; não misture tipos danificados sem orientação.", "Não abra, perfure, queime ou tente recarregar se estiver danificada.", FonteSinirPilhas),
            Item("Casca de banana", "casca-de-banana", 7, "Resíduo orgânico de alimento que pode ser compostado em sistema adequado.", "Separe de embalagens e outros rejeitos; corte em pedaços menores se for usar composteira.", "Composte em sistema apropriado ou encaminhe à coleta convencional conforme calendário e regras do município; a coleta de orgânicos pode não existir em toda localidade.", "Não coloque em recicláveis limpos e secos.", "Na compostagem, siga as instruções do sistema e evite atrair pragas mantendo a composteira coberta.", FonteMmaCompostagem),
            Item("Borra de café", "borra-de-cafe", 7, "Resíduo orgânico resultante do preparo de café, adequado à compostagem em sistema apropriado.", "Deixe escorrer o excesso de líquido e separe do filtro conforme o material e as regras locais.", "Use em composteira compatível ou descarte na coleta convencional segundo as orientações municipais.", "Não despeje grandes quantidades em ralos nem misture com recicláveis limpos.", "Mantenha o recipiente de armazenamento fechado e a composteira equilibrada conforme suas instruções.", FonteMmaCompostagem),
            Item("Cascas de ovos", "cascas-de-ovos", 7, "Resíduo orgânico de origem alimentar, que pode ser encaminhado à compostagem adequada.", "Separe de embalagens; se for compostar, quebre as cascas em pedaços menores.", "Composte em sistema apropriado ou siga a coleta convencional do município; regras e disponibilidade variam localmente.", "Não coloque em recicláveis limpos e secos.", "Mantenha restos de alimento em recipiente fechado até a compostagem ou coleta.", FonteMmaCompostagem)
        };

        Residuo Item(string nome, string slug, int categoriaId, string descricao, string preparacao, string descarte,
            string naoFazer, string cuidados, string fonte)
        {
            return new Residuo
            {
                Nome = nome,
                NomeNormalizado = ServicoPesquisaResiduos.Normalizar(nome),
                Slug = slug,
                Descricao = descricao,
                CategoriaId = categoriaId,
                Categoria = categorias[categoriaId],
                InstrucoesPreparacao = preparacao,
                InstrucoesDescarte = descarte,
                OQueNaoFazer = naoFazer,
                Cuidados = cuidados,
                FonteInformacao = fonte,
                Ativo = true
            };
        }
    }

    private static Categoria NovaCategoria(string nome, string slug, string descricao, string identificacaoVisual)
    {
        return new Categoria { Nome = nome, Slug = slug, Descricao = descricao, IdentificacaoVisual = identificacaoVisual };
    }

    private static List<Residuo> CriarResiduosIniciais(IReadOnlyDictionary<string, Categoria> categorias)
    {
        return new List<Residuo>
        {
            NovoResiduo("Garrafa PET", "garrafa-pet", "Embalagem plástica usada para bebidas.", "plasticos", categorias, "Esvazie a embalagem e retire o excesso de resíduos. Se necessário, passe água e deixe secar.", "Separe dos rejeitos e encaminhe à coleta seletiva somente se o serviço do município aceitar esse material. Confira o calendário e as regras locais.", "Não coloque a embalagem com líquido ou restos de alimentos.", "Reduza o risco de mau cheiro mantendo a embalagem vazia e, se lavada, seca.", FonteMmaReciclagem, "garrafa pet", "pet", "garrafa de plástico"),
            NovoResiduo("Papelão", "papelao", "Caixas e embalagens de papelão limpas e secas.", "papel-e-papelao", categorias, "Retire restos de alimento e mantenha o papelão seco. Dobre ou achate a caixa para ocupar menos espaço.", "Encaminhe à coleta seletiva se houver aceitação no município. Consulte o calendário e as regras da coleta local.", "Papelão engordurado ou molhado pode não ser aceito junto dos recicláveis; confirme a regra municipal.", "Cuidado com grampos e bordas ao desmontar caixas.", FonteMmaReciclagem, "papelão", "caixa de papelão", "papel cartao"),
            NovoResiduo("Lata de alumínio", "lata-de-aluminio", "Embalagem metálica de bebida ou alimento feita de alumínio.", "metais", categorias, "Esvazie e remova o excesso de resíduos. Uma passada de água é suficiente quando necessário.", "Separe para a coleta seletiva local quando ela aceitar latas de alumínio. Verifique o calendário e as regras do município.", "Não descarte a lata com restos de alimento ou líquido.", "A borda pode ser cortante; manuseie sem pressionar ou amassar com as mãos.", FonteMmaReciclagem, "lata de aluminio", "latinha", "lata de bebida"),
            NovoResiduo("Garrafa de vidro", "garrafa-de-vidro", "Embalagem de vidro comum, como garrafas e potes. Outros tipos de vidro podem ter destinação diferente.", "vidros", categorias, "Esvazie e retire o excesso de resíduos. Mantenha a embalagem inteira sempre que possível.", "Entregue à coleta seletiva apenas se o município aceitar embalagens de vidro. Consulte o calendário e as regras locais.", "Não misture lâmpadas, espelhos, louças ou vidro temperado sem verificar a orientação específica da cidade.", "Se quebrar, recolha os cacos sem contato direto, coloque-os em embalagem rígida e identifique-a para evitar cortes durante a coleta.", FontePrefeituraVidro, "garrafa de vidro", "vidro", "pote de vidro"),
            NovoResiduo("Óleo de cozinha usado", "oleo-de-cozinha-usado", "Óleo residual de preparo de alimentos. O encaminhamento depende de coleta ou ponto de entrega disponível na localidade.", "oleo-de-cozinha", categorias, "Espere esfriar completamente e coloque em uma garrafa plástica resistente, bem fechada.", "Entregue apenas a um ponto ou programa de coleta confirmado por canal oficial local. A disponibilidade e o recebimento variam por município.", "Não despeje na pia, no vaso sanitário ou diretamente no solo.", "Evite transferir óleo ainda quente para não causar queimaduras ou danificar o recipiente.", FontePrefeituraOleo, "oleo de cozinha", "oleo usado", "oleo de fritura"),
            NovoResiduo("Pilha comum", "pilha-comum", "Pilha doméstica usada. Pilhas e baterias possuem sistema próprio de logística reversa.", "pilhas-e-baterias", categorias, "Mantenha as pilhas secas e guarde-as temporariamente em recipiente que evite contato com líquidos.", "Devolva a um ponto de recebimento de pilhas e baterias divulgado pelo comércio ou por canal oficial. Confirme o local antes de se deslocar.", "Não descarte no lixo comum nem misture com a coleta seletiva de embalagens.", "Não abra, perfure ou queime a pilha. Se estiver danificada ou vazando, evite contato direto e procure orientação do serviço local.", FonteSinirPilhas, "pilha", "pilhas", "bateria pequena"),
            NovoResiduo("Restos de alimentos", "restos-de-alimentos", "Resíduos orgânicos como cascas, talos e sobras de alimentos.", "residuos-organicos", categorias, "Separe dos materiais recicláveis e mantenha em recipiente adequado até o encaminhamento.", "Se houver coleta municipal de orgânicos, siga o calendário e as regras locais. A compostagem é uma possibilidade quando feita em sistema apropriado.", "Não misture restos orgânicos com embalagens recicláveis limpas e secas.", "Mantenha o recipiente fechado e a composteira coberta conforme as instruções do sistema utilizado.", FonteMmaCompostagem, "restos de comida", "casca de fruta", "residuo organico")
        };
    }

    private static Residuo NovoResiduo(string nome, string slug, string descricao, string categoriaSlug, IReadOnlyDictionary<string, Categoria> categorias, string preparacao, string descarte, string naoFazer, string cuidados, string fonte, params string[] sinonimos)
    {
        var categoria = categorias[categoriaSlug];
        var residuo = new Residuo
        {
            Nome = nome,
            NomeNormalizado = ServicoPesquisaResiduos.Normalizar(nome),
            Slug = slug,
            Descricao = descricao,
            Categoria = categoria,
            CategoriaId = categoria.Id,
            InstrucoesPreparacao = preparacao,
            InstrucoesDescarte = descarte,
            OQueNaoFazer = naoFazer,
            Cuidados = cuidados,
            FonteInformacao = fonte,
            Ativo = true
        };

        residuo.Sinonimos = sinonimos.Select(nomeSinonimo => new Sinonimo
        {
            Nome = nomeSinonimo,
            NomeNormalizado = ServicoPesquisaResiduos.Normalizar(nomeSinonimo)
        }).ToList();

        return residuo;
    }
}
